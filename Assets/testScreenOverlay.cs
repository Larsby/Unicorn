using System;
using UnityEngine;

namespace UnityStandardAssets.ImageEffects
{
	[ExecuteInEditMode]
	[RequireComponent (typeof(Camera))]
	[AddComponentMenu ("TestScreenOverlay")]
	public class testScreenOverlay : PostEffectsBase
	{
		 

		public Shader myShader = null;
		private Material myMaterial = null;


		public override bool CheckResources ()
		{
			CheckSupport (false);
			myMaterial = CheckShaderAndCreateMaterial(myShader,myMaterial);

			if (!isSupported)
				ReportAutoDisable ();
			return isSupported;
		}

		void OnRenderImage (RenderTexture source, RenderTexture destination)
		{
			if (CheckResources()==false)
			{
				Graphics.Blit (source, destination);
				return;
			}
			  
			Graphics.Blit (source, destination, myMaterial);
		}
	}
}


