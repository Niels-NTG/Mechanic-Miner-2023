using System;

public record GeneticAlgorithmLogRow
{
    public int generation { get; set; }
    public String lineageId { get; set; }
    public String id { get; set; }
    public int hash { get; set; }
    public double fitness { get; set; }
    public String gameObject { get; set; }
    public String component { get; set; }
    public String componentField { get; set; }
    public String modifier { get; set; }
    public String fieldType { get; set; }
    public int iterations { get; set; }
    public int archiveLength { get; set; }
    public String archive { get; set; }
    public String terminalTrajectories { get; set; }
}
