using System;
using UnityEngine;
using UnityEngine.UI;
using Application = UnityEngine.Application;

namespace Source.Interfaces
{
    [ExecuteInEditMode]
    public class PrintVersionNumber:MonoBehaviour
    {
        private Text _text;
        private void Start()
        {
            _text = GetComponent<Text>();
        }

    #if UNITY_EDITOR
    
        void Update()
        {
            _text.text = String.Concat("Version: ", Application.version,"\nBuild with:",Application.unityVersion);
        }
    #endif
    }
 }
