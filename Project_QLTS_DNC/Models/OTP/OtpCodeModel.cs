using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_QLTS_DNC.Models.OTP
{
    [Table("otp_codes")]
    public class OtpCodeModel : BaseModel
    {
        [PrimaryKey("id", false)]
        public long Id { get; set; }

        [Column("email")]
        public string Email { get; set; }

        [Column("otp_code")]
        public string OtpCode { get; set; }

        [Column("expire_at")]
        public DateTime ExpireAt { get; set; }

        [Column("verified")]
        public bool Verified { get; set; } = false;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
