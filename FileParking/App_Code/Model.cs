using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FileParking.Models1
{
    public class Member
    {
        public Guid ID { get; set; }
        [Required]
        public string Email { get; set; }
        [Required, MaxLength(300), MinLength(8)]
        public string Password { get; set; }
        [Required(AllowEmptyStrings = true), MaxLength(50)]
        public string FirstName { get; set; }
        [MaxLength(50)]
        public string LastName { get; set; }
        [Required, MaxLength(500)]
        public string Folder { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateModified { get; set; }

        public virtual ICollection<ParkedFile> ParkedFiles { get; set; }
    }

    public class ParkedFile
    {
        public Guid ID { get; set; }
        public string FileName { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime ExpiryDate { get; set; }
    }

    public class Recipient
    {
        public int ID { get; set; }
        [Required, MaxLength(100)]
        public string Name { get; set; }
        [Required, MaxLength(250)]
        public string Email { get; set; }
    }

    public class Transfer
    {
        public int ID { get; set; }
        public string Message { get; set; }
        public string Subject { get; set; }
        public virtual Member Sender { get; set; }
        public virtual Recipient Recipient { get; set; }
        public virtual ICollection<ParkedFile> ParkedFiles { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateRead { get; set; }
    }
}