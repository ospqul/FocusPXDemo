namespace FPXDemo.Models
{
    public interface IProbeModel
    {
        double Frequency { get; set; }
        double Pitch { get; set; }
        uint TotalElements { get; set; }
        uint UsedElementsPerBeam { get; set; }
    }
}