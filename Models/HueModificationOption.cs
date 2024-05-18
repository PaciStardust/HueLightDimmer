namespace HueLightDimmer.Models
{
    internal class HueModificationOption
    {
        public HueModificationType ModificationType { get; set; }
        public int ModificationValue { get; set; }
    }

    internal enum HueModificationType
    {
        None,
        Set,
        Modify
    }
}
