using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Phenix.Core
{
    public  class ParamSupport
    {
        public int min_groups = int.MaxValue;
        List<Param> paramProList;
        Param paramPro;
        int group;
        public ParamSupport(List<Param> paramProList)
        {
            this.paramProList = paramProList;
        }
        public List<string[]> getParams()
        {
            List<string[]> param = new List<string[]>();
            foreach(Param pitem in this.paramProList)
            {
                this.paramPro = pitem;
                this.group = (int)Math.Ceiling(pitem.aArray.Count*1.0 / (pitem.unit*1.0));
                min_groups = min_groups > group ? group : min_groups;
                param.Add(getParam());
            }
            return param;
        }
        public string[] getParam()
        {
            if (group <= 0)
            {
                return null;
            }
            string[] param = new string[group];
            switch (paramPro.type)
            {
                case (int)EnumParams.Array:
                    param = ArrayParam();
                    break;
                case (int)EnumParams.FileList:
                    
                    break;
                case (int)EnumParams.Database:

                    break;
            }
            return param;
        }
        public  string[] ArrayParam()
        {
            string[] param = new string[group];
            int k = 0;

            for (int i = 0; i <  paramPro.aArray.Count; i++)
            {
                if (param[k] == null)
                {
                    param[k] = paramPro.aArray[i];
                }
                else
                {
                    param[k] = param[k] + paramPro.seperator + paramPro.aArray[i];
                }
                if ((i+1)%paramPro.unit == 0 )
                {
                    k++;
                }
            }
            if(k != group)
            {
                paramPro.Exceptions.Add(new Exception("Array not match unit"));
            }
            return param;
        }
        public enum EnumParams
        {
            Array = 0,
            FileList = 1,
            Database = 2
        }
        public List<int> getFormatSeq(string inputParamsFormat)
        {
            List<int> seq = new List<int>();
            Regex r = new Regex(@"\{(\d*)\}");
            Regex digit = new Regex(@"\d");
           
            MatchCollection mc = r.Matches(inputParamsFormat);
            for (int i = 0; i < mc.Count; i++)  
            {
                Match m = digit.Match(mc[i].Value);
                seq.Add(int.Parse(m.Value));
            }
            return seq;
        }
    }
}
