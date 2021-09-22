using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Scripting;

namespace Didimo.GLTFUtility
{
	[Preserve]
	public class DidimoExtension
	{
		public List<GLTFTexture>       textures;
		public List<GLTFImage>         images;
		public string                  version                 = "2.5";
		public string                  headJoint               = "Head";
		public DidimoEyeShadowSettings didimoEyeShadowSettings = new DidimoEyeShadowSettings();

		[Preserve]
		public class DidimoEyeShadowSettings
		{
			public string[] leftEyelidJoints =
			{
				"DemLuUpperEyelidLevelTwo01", "DemLuUpperEyelidLevelTwo02", "DemLuUpperEyelidLevelTwo03", "DemLuUpperEyelidLevelTwo04", "DemLuUpperEyelidLevelTwo05",
				"DemLuUpperEyelidLevelTwo06", "DemLuUpperEyelidLevelTwo07", "DemLuUpperEyelidLevelTwo08", "DemLdLowerEyelidLevelTwo08", "DemLdLowerEyelidLevelTwo07",
				"DemLdLowerEyelidLevelTwo06", "DemLdLowerEyelidLevelTwo05", "DemLdLowerEyelidLevelTwo04", "DemLdLowerEyelidLevelTwo02", "DemLdLowerEyelidLevelTwo01",
			};

			public string[] rightEyelidJoints =
			{
				"DemRuUpperEyelidLevelTwo01", "DemRuUpperEyelidLevelTwo02", "DemRuUpperEyelidLevelTwo03", "DemRuUpperEyelidLevelTwo04", "DemRuUpperEyelidLevelTwo05",
				"DemRuUpperEyelidLevelTwo06", "DemRuUpperEyelidLevelTwo07", "DemRuUpperEyelidLevelTwo08", "DemRdLowerEyelidLevelTwo08", "DemRdLowerEyelidLevelTwo07",
				"DemRdLowerEyelidLevelTwo06", "DemRdLowerEyelidLevelTwo05", "DemRdLowerEyelidLevelTwo04", "DemRdLowerEyelidLevelTwo02", "DemRdLowerEyelidLevelTwo01"
			};

			public string headJoint = "Head";


			public string leftEyeMesh  = "mesh_l_low_eye_001";
			public string rightEyeMesh = "mesh_r_low_eye_001";
		}

		public void Build(GLTFNode.ImportResult[] nodeResult, GLTFAnimation.ResetAnimationImportResult resetAnimationResult, ref Importer.ImportResult importResult)
		{
			if (resetAnimationResult != null) importResult.resetAnimationClip = resetAnimationResult.clip;

			importResult.headJoint = nodeResult.First(import => import.transform.name == headJoint).transform;

			importResult.eyeShadowController = new Importer.EyeShadowControllerSettings();
			importResult.eyeShadowController.HeadJoint = nodeResult.First(import => import.transform.name == didimoEyeShadowSettings.headJoint).transform;
			importResult.eyeShadowController.LeftEyeMesh = nodeResult.First(import => import.transform.name == didimoEyeShadowSettings.leftEyeMesh)
																	.transform.gameObject.GetComponent<SkinnedMeshRenderer>();
			importResult.eyeShadowController.RightEyeMesh = nodeResult.First(import => import.transform.name == didimoEyeShadowSettings.rightEyeMesh)
																	.transform.gameObject.GetComponent<SkinnedMeshRenderer>();

			importResult.irisController = new Importer.IrisControllerSettings();
			importResult.irisController.LeftEyeMesh = importResult.eyeShadowController.LeftEyeMesh;
			importResult.irisController.RightEyeMesh = importResult.eyeShadowController.RightEyeMesh;

			SetTransforms(nodeResult, didimoEyeShadowSettings.leftEyelidJoints, out importResult.eyeShadowController.LeftEyelidJoints);
			SetTransforms(nodeResult, didimoEyeShadowSettings.rightEyelidJoints, out importResult.eyeShadowController.RightEyelidJoints);
		}

		private static void SetTransforms(GLTFNode.ImportResult[] nodeResult, string[] transformNames, out Transform[] transforms)
		{
			transforms = new Transform[transformNames.Length];
			for (int i = 0; i < transformNames.Length; i++)
			{
				transforms[i] = nodeResult.First(import => import.transform.name == transformNames[i]).transform;
			}
		}
	}
}