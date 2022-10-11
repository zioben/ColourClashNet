﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color
{
    public class ColorTransformQuantization: ColorTransformBase
    {

        public ColorQuantizationMode QuantizationMode { get; set; }
        protected override void BuildTrasformation()
        {
            DictTransform.Clear();          
            foreach (var kvp in DictHistogram)
            {
                if( !kvp.Key.Equals(BackColor))
                    DictTransform.Add(kvp.Key, kvp.Key.Quantize(QuantizationMode));
            }
        }
    }
}
