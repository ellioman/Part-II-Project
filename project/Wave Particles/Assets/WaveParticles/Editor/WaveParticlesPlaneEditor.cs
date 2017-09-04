using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(WaveParticlePlane))]
public class WaveParticlePlaneEditor : Editor
{
    public override void OnInspectorGUI()
    {
        WaveParticlePlane waveParticles = (WaveParticlePlane)target;

        /// TODO: Make the undo system work with all facets of wave Particles

        /////
        ///// Show wave particles initialisation info that can be shown, this stuff can only be updated pre initialisation
        ///// 
        //if (waveParticles.hasStarted)
        //{

        //    // Update number of Wave Particles

        //    // Change the number 
        //}


        ///
        /// Everything that can updated after-initialisation
        /// 
        {
            // Wave Particle Kill Threhold
            EditorGUI.BeginChangeCheck();
            float waveParticleKillThreshold = EditorGUILayout.FloatField("Wave Particle Kill Threshold", waveParticles.waveParticleKillThreshold);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Changed the WaveParticleKillThreshold");
                waveParticles.waveParticleKillThreshold = waveParticleKillThreshold;
            }
        }


        ///
        /// Decide which vertex-displacement method to use
        ///
        {
            waveParticles.useGpuForVertices = EditorGUILayout.Toggle("Use Gpu For Vertices: ", waveParticles.useGpuForVertices);
        }


        ///
        /// Show available Height Field Generators
        ///
        {
            waveParticles.selectedHeightFieldGenerator = (HeightFieldGeneratorSelector.Choice)EditorGUILayout.EnumPopup(waveParticles.selectedHeightFieldGenerator);
        }

        ///
        /// Show available debug textures that can be shown
        ///
        {
            var enabledTextures = waveParticles.enabledDisplayTextures;
            for (int i = 0; i < enabledTextures.Length; i++)
            {
                bool enabled = EditorGUILayout.Toggle("Show " + enabledTextures[i].first, enabledTextures[i].second);
                enabledTextures[i] = new StringBoolPair(enabledTextures[i].first, enabled);
            }
        }
    }
}
