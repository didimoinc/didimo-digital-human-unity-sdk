using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework.Constraints;
using UnityEditor;
using UnityEngine;

namespace Didimo.Mobile.Communication
{
    public class UnityInterfaceGenerator
    {
#if UNITY_IOS
        private const string hash = "#";

        [MenuItem("Didimo/Tools/Generate Didimo iOS Interface")]
        public static void GenerateIOSUnityInterface()
        {
            HashSet<Type> iosDelegates = new HashSet<Type>();
            List<string> objCImplementations = new List<string>();
            List<string> externCImplelemtations = new List<string>();
            List<string> objCInterfaces = new List<string>();
            IEnumerable<Type> nativeInterfaces = BiDirectionalNativeInterface.GetAllNativeInterfaces();
            foreach (var type in nativeInterfaces)
            {
                Generate(type, out string objCImpl, out string externCImpl, out string objCInterf, ref iosDelegates);
                objCImplementations.Add(objCImpl);
                externCImplelemtations.Add(externCImpl);
                objCInterfaces.Add(objCInterf);
            }

            List<string> objcDelegates = new List<string>();
            foreach (Type iosDelegateType in iosDelegates)
            {
                List<string> parameters = new List<string>();
                foreach (var parameter in iosDelegateType.GetMethod("Invoke")!.GetParameters())
                {
                    parameters.Add($"{ObjCDelegateArgument(parameter.ParameterType)} {parameter.Name}");
                }

                objcDelegates.Add($"typedef void (*{iosDelegateType.Name})({string.Join(", ", parameters)});");
            }

            string iOSImplementation = $@"/************** GENERATED AUTOMATICALLY **************/
{hash}import <Foundation/Foundation.h>
{hash}import ""DidimoUnityInterface.h""

@implementation DidimoUnityInterface

const char ** cCharArrayFromNSArray ( NSArray* array ){{
    unsigned long i, count = array.count;
    const char **cargs = (const char**) malloc(sizeof(char*) * (count + 1));
    for(i = 0; i < count; i++) {{        //cargs is a pointer to 4 pointers to char
        NSString *s      = array[i];     //get a NSString
        const char *cstr = s.UTF8String; //get cstring
        unsigned long        len = strlen(cstr); //get its length
        char  *cstr_copy = (char*) malloc(sizeof(char) * (len + 1));//allocate memory, + 1 for ending '\0'
        strcpy(cstr_copy, cstr);         //make a copy
        cargs[i] = cstr_copy;            //put the point in cargs
    }}
    return cargs;
}}

{string.Join("\n", objCImplementations)}


@end

{hash}if __cplusplus
extern ""C"" {{
{hash}endif
      
{string.Join("\n", externCImplelemtations)}
{hash}if __cplusplus

}}
{hash}endif

";
            string iosInterface = $@"/************** GENERATED AUTOMATICALLY **************/


{hash}import <Foundation/Foundation.h>

NS_ASSUME_NONNULL_BEGIN

__attribute__ ((visibility(""default"")))
@interface DidimoUnityInterface : NSObject{{}}

{string.Join("\n", objcDelegates)}

{string.Join("\n", objCInterfaces)}

@end

NS_ASSUME_NONNULL_END
";
            File.WriteAllText("Assets/Didimo/Mobile/Plugins/iOS/DidimoUnityInterface.m", iOSImplementation);
            File.WriteAllText("Assets/Didimo/Mobile/Plugins/iOS/DidimoUnityInterface.h", iosInterface);
        }

        private static string ObjCDelegateArgument(Type type)
        {
            if (type.HasElementType) throw new Exception("Arrays are not supported. User IntPtr and do the Marshalling manually.");
            
            if (type == typeof(string)) return "const char*";

            if (type == typeof(int)) return "int";

            if (type == typeof(float)) return "float";


            if (type == typeof(IntPtr)) return "const void*";

            return type!.Name;
        }
        
        private static string ObjCMethodArgument(Type type)
        {
            if (type.HasElementType)
            {
                throw new Exception("Arrays are not supported. User IntPtr and do the Marshalling manually.");
                if (type == typeof(string[])) return "NSArray*";

                return ObjCMethodArgument(type.GetElementType()) + "[_Nonnull]";
            }

            if (type == typeof(string)) return "NSString*";
            
            if (type == typeof(int)) return "int";

            if (type == typeof(float)) return "float";
            
            if (type == typeof(IntPtr)) return "const void*";

            return type.Name;
        }

        private static string CToObjCArgument(Type type, string argumentName)
        {
            if (type == typeof(string)) return $"[{argumentName} UTF8String]";
            
            if (type == typeof(string[])) return $"cCharArrayFromNSArray({argumentName})";


            return argumentName;
        }

        private static void Generate(Type type, out string objCImpl, out string externCImpl, out string objCInterf, ref HashSet<Type> iosDelegates)
        {
            objCImpl = objCImplementation;
            externCImpl = externCImplementation;
            objCInterf = objCInterface;

            MethodInfo registerMethod = type.GetRuntimeMethods().First(mI => mI.Name.StartsWith("register"));
            if (registerMethod == null)
            {
                throw new Exception($"Could not find register method for type {type}.");
            }

            ParameterInfo[] registerParameters = registerMethod.GetParameters();
            if (registerParameters.Length != 1)
            {
                throw new Exception($"Register method should only have a single parameter (the input delegate). This was not the case for ${type}");
            }

            Type inputParameterType = registerParameters.First().ParameterType;

            List<string> delegateArguments = new List<string>();
            List<string> methodArguments = new List<string>();
            List<string> cToObjcArguments = new List<string>();
            bool first = true;
            foreach (ParameterInfo parameter in inputParameterType.GetMethod("Invoke")!.GetParameters())
            {
                // If this parameter is a delegate, add it to the list of delegates we will have to declare int he Objective-C interface 
                if (parameter.ParameterType.GetMethod("Invoke") != null)
                {
                    iosDelegates.Add(parameter.ParameterType);
                }

                string objCDelegateArgument = ObjCDelegateArgument(parameter.ParameterType);
                delegateArguments.Add($"{objCDelegateArgument} {parameter.Name}");
                string objCMethodArgument = ObjCMethodArgument(parameter.ParameterType);
                string prefix = first ? "" : parameter.Name;
                methodArguments.Add($"{prefix}:({objCMethodArgument}){parameter.Name}");

                cToObjcArguments.Add(CToObjCArgument(parameter.ParameterType, parameter.Name));

                first = false;
            }

            objCImpl = objCImpl.Replace("{CALLBACK_ARGUMENTS}", String.Join(", ", delegateArguments));
            objCImpl = objCImpl.Replace("{OBJC_ARGUMENTS}", String.Join(" ", methodArguments));
            objCImpl = objCImpl.Replace("{C_TO_OBJC_ARGUMENTS}", String.Join(", ", cToObjcArguments));
            objCImpl = objCImpl.Replace("{METHOD_NAME}", type.Name);

            string methodVarName = type.Name;
            if (methodVarName[0].ToString().ToLower() != methodVarName[0].ToString())
            {
                methodVarName = methodVarName[0].ToString().ToLower() + methodVarName.Substring(1); // Make first character lower case
            }
            else
            {
                methodVarName = "_" + methodVarName;
            }

            objCImpl = objCImpl.Replace("{METHOD_VAR_NAME}", methodVarName);

            externCImpl = externCImpl.Replace("{METHOD_NAME}", type.Name);

            objCInterf = objCInterf.Replace("{METHOD_NAME}", type.Name);
            objCInterf = objCInterf.Replace("{OBJC_ARGUMENTS}", String.Join(" ", methodArguments));
        }

        static IEnumerable<Type> GetTypesWithAttribute(Assembly assembly, Type attribute)
        {
            foreach (Type type in assembly.GetTypes())
            {
                if (type.GetCustomAttributes(attribute, true).Length > 0)
                {
                    yield return type;
                }
            }
        }

        const string objCImplementation = @"
typedef void (*{METHOD_NAME}Cb)({CALLBACK_ARGUMENTS});
static {METHOD_NAME}Cb {METHOD_VAR_NAME}Cb;
+ (void) register{METHOD_NAME}:({METHOD_NAME}Cb)cb{
    
    {METHOD_VAR_NAME}Cb = cb;
}
+ (void) {METHOD_NAME}{OBJC_ARGUMENTS}{
    
    {METHOD_VAR_NAME}Cb({C_TO_OBJC_ARGUMENTS});
}
";

        const string externCImplementation = @"    
    void register{METHOD_NAME}({METHOD_NAME}Cb cb) {
 
        [DidimoUnityInterface register{METHOD_NAME}: cb];
    }";

        const string objCInterface = @"
+ (void) {METHOD_NAME}{OBJC_ARGUMENTS};
";
#endif
    }
}