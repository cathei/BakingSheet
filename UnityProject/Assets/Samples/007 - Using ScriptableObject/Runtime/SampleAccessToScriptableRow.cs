using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Cathei.BakingSheet.Examples
{
    public class SampleAccessToScriptableRow : MonoBehaviour
    {
        public HeroSheet.Reference reference;

        public void Awake()
        {
            Debug.Log(reference.Ref.Name);
        }
    }
}
