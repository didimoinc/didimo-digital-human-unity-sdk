using System.Collections.Generic;
using UnityEngine.Scripting;

namespace Didimo.GLTFUtility
{
	[Preserve]
	public class DidimoExtension
	{
		public List<GLTFTexture> textures;
		public List<GLTFImage>   images;
		public string            version;
	}
}