using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
namespace WeddingPlanner.Models
{
    public class Association
    {
        [Key]
        public int AssociationId { get; set; }
        [Required]
        public int WeddingId { get; set; }
        [Required]
        public int UserId { get; set; }
        public User User { get; set; }
        public Wedding Wedding { get; set; }
    }
}