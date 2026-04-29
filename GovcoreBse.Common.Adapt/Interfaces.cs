using System;

namespace GovcoreBse.Common.Adapt
{


    public static class Interfaces
    {
        public interface IFilePondLoadRequest
        {
            string[] InUrls { get; set; }
            string[] OutUrls { get; set; }

            string Source { get; set; }
        }

        public interface IFilePondPreLoadRequest
        {
            string PondAttachTypeName { get; set; }
            string PondAttachType { get; set; }
        }

        public interface IUrl
        {
            long Size { get; set; }
            bool CanLoad { get; set; }
            string Caption { get; set; }
            string Url { get; set; }
            string Type { get; set; }
            string Name { get; set; }
            long Thumb { get; set; }
        }

        public interface IUrlModel
        {
            IUrl[] Urls { get; set; }
            int MaxCount { get; set; }
            string BaseUrl { get; set; }
            string BaseUrlByName { get; set; }
            string UrlTitle { get; set; }
            int InitStart { get; set; }

            IUrlModel ExtractModel(int init, int len);
        }
    }
}

