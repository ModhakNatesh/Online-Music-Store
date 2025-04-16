using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OnlineMusicStore.Models
{
    public class Genre
    {
        [Key]
        public int GenreId { get; set; }

        [Required]
        public string Name { get; set; }

        public virtual ICollection<MusicItem> MusicItems { get; set; }
    }
}
