using System;

public record GeneticAlgorithmLogRow
{
    public int generation { get; set; }
    public String id { get; set; }
    public double fitness { get; set; }
    public String gameObject { get; set; }
    public String component { get; set; }
    public String componentField { get; set; }
    public String modifier { get; set; }
}
