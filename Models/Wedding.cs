using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace WeddingPlanner.Models
{
    public class Wedding
    {
        [Key]
        [Required]
        public int WeddingId { get; set; }
        [Required]
        public string WedderOne { get; set; }
        [Required]
        public string WedderTwo { get; set; }
        [Required]
        public DateTime WeddingDate { get; set; }
        [Required]
        public string Address { get; set; }
        public int UserId {get;set;}
        public User Creator {get;set;}
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // public Boolean Rsvp {get; set;}

        // 
        public List<Association> UsersInThisWedding { get; set; }
    }
}