using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineMusicStore.Models
{
    public class MusicItem
    {
        [Key]
        public int MusicItemId { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Artist { get; set; }

        public string Genre { get; set; }

        [Required]
        [Display(Name = "Release Date")]
        [DataType(DataType.Date)]
        public DateTime ReleaseDate { get; set; }

        [Required]
        [Display(Name = "Available Stock")]
        public int Stock { get; set; }


        [Required]
        public decimal Price { get; set; }

        public string ImageUrl { get; set; }

        public virtual ICollection<Vote> Votes { get; set; } = new List<Vote>();

        public virtual ICollection<Rating> Ratings { get; set; }



    }
}
