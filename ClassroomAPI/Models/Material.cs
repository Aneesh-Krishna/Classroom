namespace ClassroomAPI.Models
{
    public class Material
    {
        public Guid MaterialId { get; set; } = Guid.NewGuid();
        public string MaterialName { get; set; } = string.Empty;
        public string MaterialUrl { get; set; } = string.Empty;
        public Guid CourseId { get; set; }
        public Course? Course { get; set; }
    }
}
