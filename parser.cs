using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OcrTsvToJson
{
    public class parser
    {
        private string json;
        public parser(string ocrresult)
        {
            ParseResult(ocrresult);
        }
        public string getResult()
        {
                return json;
        }
        private void ParseResult(string ocrresult)
        {
            try
            {
                string[] lines = ocrresult.Trim().Split('\n');
                List<OcrParamList> param = new List<OcrParamList>();
                int ocrlines = lines.Length;
                foreach (string line in lines)
                {
                    OcrParamList ocp = new OcrParamList();
                    string[] sublines = line.Split('\t');
                    //ocp.level = Convert.ToInt32(sublines[0]);
                    // ocp.page_num = Convert.ToInt32(sublines[1]);
                    //ocp.block_num= Convert.ToInt32(sublines[2]);
                    // ocp.par_num = Convert.ToInt32(sublines[3]);
                    // ocp.line_num = Convert.ToInt32(sublines[4]);
                    // ocp.word_num= Convert.ToInt32(sublines[5]);
                    ocp.left = Convert.ToInt32(sublines[6]);
                    ocp.top = Convert.ToInt32(sublines[7]);
                    ocp.width = Convert.ToInt32(sublines[8]);
                    ocp.height = Convert.ToInt32(sublines[9]);
                    // ocp.conf = Convert.ToInt32(sublines[10])<0?0: Convert.ToInt32(sublines[10]);
                    ocp.text = sublines[11].Trim('\r');
                    param.Add(ocp);
                }
                json = JsonConvert.SerializeObject(param);
            }
            catch(Exception ex)
            {
                throw ex;
            }
           
        }
    }
    internal class OcrParamList
    {
        //public int level { get;  set; }
        //public int page_num { get;  set; }
        //public int block_num { get;  set; }
        //public int par_num { get;  set; }
        //public int line_num { get;  set; }
        //public int word_num { get;  set; }
        public int left { get; set; }
        public int top { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        // public int conf { get;  set; }
        public string text { get; set; }
    }
}
