using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisitTracker.Models
{
    public class Member
    {
        public int ID { get; set; }
        [MaxLength(50)]
        public string FirstName { get; set; }
        [MaxLength(50)]
        public string LastName { get; set; }
        [Required, MaxLength(250)]
        public string Email { get; set; }
        [Required, MaxLength(250)]
        public string Password { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateModified { get; set; }
        public RecordStatus Status { get; set; }

        public Guid PublicId { get; set; }

    }
}
