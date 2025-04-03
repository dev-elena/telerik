using telerik_Q1_25.Pages.Dtos.SelectionAndPricing.Enumerations;

namespace telerik_Q1_25.Pages.Dtos.SelectionAndPricing
{
    public class SizingEnumeration : BaseEnumeration
    {
        public static readonly SizingEnumeration Width = new(1, "Width");
        public static readonly SizingEnumeration Height = new(2, "Height");
        public static readonly SizingEnumeration Length = new(3, "Length");
        public static readonly SizingEnumeration Diameter = new(4, "Diameter");
        public static readonly SizingEnumeration Slots = new(5, "Slots");

        public static readonly SizingEnumeration SectionText = new(6, "SectText");
        public static readonly SizingEnumeration NumberOfSections = new(7, "numSects");

        public static readonly SizingEnumeration SectionSizeHigh = new(8, "sectH");
        public static readonly SizingEnumeration SectionSizeWide = new(9, "sectW");
        public static readonly SizingEnumeration SectionSizeLength = new(10, "sectS");
        public static readonly SizingEnumeration SectionSizeDiameter = new(11, "sectD");
        public static readonly SizingEnumeration SectionSizeSlots = new(12, "sectSlots");

        public static readonly SizingEnumeration NumberSectionsHorizontal = new(13, "NumSectH");
        public static readonly SizingEnumeration NumberSectionsVertical = new(14, "NumSectV");

        public static readonly SizingEnumeration MaxSectionSizeHigh = new(15, "maxSectH");
		public static readonly SizingEnumeration MaxSectionSizeHighA = new(15, "maxSectHA");
		public static readonly SizingEnumeration MaxSectionSizeWideA = new(16, "maxSectWA");
		public static readonly SizingEnumeration MaxSectionSizeHighB = new(15, "maxSectHB");
		public static readonly SizingEnumeration MaxSectionSizeWideB = new(16, "maxSectWB");
		public static readonly SizingEnumeration MaxSectionSizeWide = new(16, "maxSectW");
        public static readonly SizingEnumeration MaxSectionSizeLength = new(17, "maxSectS");
        public static readonly SizingEnumeration MaxSectionSizeDiamter = new(18, "maxSectD");
        public static readonly SizingEnumeration MaxSectionSizeSlots = new(19, "maxSectSlots");

        public static readonly SizingEnumeration MinSectionSizeHigh = new(20, "minSectH");
        public static readonly SizingEnumeration MinSectionSizeWide = new(21, "minSectW");
        public static readonly SizingEnumeration MinSectionSizeLength = new(22, "minSectS");
        public static readonly SizingEnumeration MinSectionSizeDiamter = new(23, "minSectD");
        public static readonly SizingEnumeration MinSectionSizeSlots = new(24, "minSectSlots");

        public static readonly SizingEnumeration MinHeight = new(25, "minHeight");
        public static readonly SizingEnumeration MinWidth = new(26, "minWidth");
        public static readonly SizingEnumeration MinLength = new(27, "minLength");
        public static readonly SizingEnumeration MinDiameter = new(28, "minDiameter");
        public static readonly SizingEnumeration MinSlots = new(29, "minSlots");

        public static readonly SizingEnumeration MaxHeight = new(30, "maxHeight");
        public static readonly SizingEnumeration MaxWidth = new(31, "maxWidth");
        public static readonly SizingEnumeration MaxLength = new(32, "maxLength");
        public static readonly SizingEnumeration MaxDiameter = new(33, "maxDiameter");
        public static readonly SizingEnumeration MaxSlots = new(34, "maxSlots");

        public static readonly SizingEnumeration MaxSleeveSectionWidth = new(35, "maxSectSlvW");
        public static readonly SizingEnumeration MaxSleeveSectionHeight = new(36, "maxSectSlvH");
		public static readonly SizingEnumeration SleeveSectionWidth = new(35, "sectSlvW");
		public static readonly SizingEnumeration SleeveSectionHeight = new(36, "sectSlvH");

		public SizingEnumeration() { }

        public SizingEnumeration(int value, string displayName) : base(value, displayName) { }
    }
}
