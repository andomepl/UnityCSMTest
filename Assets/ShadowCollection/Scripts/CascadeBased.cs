using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace MyCascaedShadow
{

    public delegate void DataUpdate(int s, int n);

    [Serializable]
    public class CascadeSettings
    {




        [Serializable]
        public enum CascadeSize : int
        {
            Min=512,
            Mid=1024,
            Max=2048,

        }

        [Serializable]
        public enum CascadeNum : int
        {

            NoCascade=1,
            TwoCascade=2,
            FourCascade=4

        }

        [Range(0.0f, 1.0f)]
        [SerializeField]
        private float slide1;

        [Range(0.0f, 1.0f)]
        [SerializeField]
        private float slide2;


        [Range(0.0f, 1.0f)]
        [SerializeField]
        private float slide3;


        [Range(0.0f, 1.0f)]
        [SerializeField]
        private float slide4;



        [SerializeField]
        private CascadeSize m_casacdesize = CascadeSize.Mid;

        [SerializeField]
        [Tooltip("Nums of the cascade shadow split")]
        private CascadeNum m_split = CascadeNum.FourCascade;


        private TextureFormat m_textureformat = TextureFormat.RHalf;

        public int casacdesize
        {
            get => (int)m_casacdesize;
        }

        public int split
        {
            get => (int)m_split;
        }


        public TextureFormat textureFormat
        {
            get => m_textureformat;
        }



        public DataUpdate dataUpdate;

    }

}
