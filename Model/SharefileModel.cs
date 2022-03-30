using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace A2B_APP_Extension.Model
{
    class SharefileModel
    {
    }
    public class SharefileRecordings
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [JsonIgnore]
        public string Id { get; set; }
        public string SfItemId { get; set; }
        public string Videoname { get; set; }
        public string Sflink { get; set; }
        public string Keyreport { get; set; }
        public string Date { get; set; }
        public string Execute_transcription { get; set; }
        public string Execute_screenshots { get; set; }

    }

    public class SharefileUser
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Subdomain { get; set; }
        public string ControlPlane { get; set; }
    }

    public class SharefileItem
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string Directory { get; set; }
        public string ClientName { get; set; }
    }
}
