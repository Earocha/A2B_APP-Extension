using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace A2B_APP_Extension.Model
{
    public class CrawlerModel
    {
    }
    public class WebCrawler
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [JsonIgnore]

        private int Id { get; set; }
        public string Client { get; set; }
        public string WebLink { get; set; }
        public string ClickLink { get; set; }
        public string Title { get; set; }
        public string ShortDescription { get; set; }
        public string Body { get; set; }
        public string Screenshot { get; set; }
        public string SkypeGC { get; set; }
        public string Email { get; set; }
        public DateTimeOffset Create_at { get; set; } = DateTime.Now;

        public WebCrawlerScheduleTime WebCrawlerScheduleTime { get; set; }
    }

    public class WebCrawlerScheduleTime
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [JsonIgnore]

        private int Id { get; set; }
        public int ClientFkey { get; set; }
        public string Day { get; set; }
        public string Month { get; set; }
        public string Time { get; set; }
        public DateTimeOffset Created_at { get; set; } = DateTime.Now;
    }
}
