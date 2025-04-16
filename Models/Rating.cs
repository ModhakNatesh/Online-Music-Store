using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineMusicStore.Models
{
    public class Rating
    {
        [Key]
        public int RatingId { get; set; }

        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        public int MusicItemId { get; set; }

        [ForeignKey("MusicItemId")]
        public virtual MusicItem MusicItem { get; set; }

        [Range(1, 5)]
        public int Stars { get; set; }

        public string Review { get; set; }
    }
}
