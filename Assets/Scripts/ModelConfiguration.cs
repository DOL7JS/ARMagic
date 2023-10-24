using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
    [Serializable]
    public class ModelConfiguration 
    {
        public string name;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
        public ModelConfiguration(string name,Vector3 position, Quaternion rotation,Vector3 scale){
            this.name = name;
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
        }
    }