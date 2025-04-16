using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineMusicStore.Models
{
    public class Vote
    {
        [Key]
        public int VoteId { get; set; }

        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        public int MusicItemId { get; set; }

        [ForeignKey("MusicItemId")]
        public virtual MusicItem MusicItem { get; set; }
    }
}
