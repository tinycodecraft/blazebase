using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GovcoreBse.Store.Setup.Dtos
{
    public class FileItemDto: IN.IDocItem
    {
        public long Id { get; set; }
        public string DocType { get; set; }
        public string UploadFilePath { get; set; }
        public string RelativePath { get; set; }
    }
}
