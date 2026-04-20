namespace ArderBackend.Models
{
    public class Exercise
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string MainMuscle { get; set; } = string.Empty;
        public string Equipment { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string BodyPart { get; set; } = string.Empty;
        public List<string> SecondaryMusclesList { get; set; } = new List<string>();
        public List<string> Instructions { get; set; } = new List<string>();
        public string Description { get; set; } = string.Empty;
        public string Difficulty { get; set; } = string.Empty;
    }
}
