using System;

namespace telerik_Q1_25.Pages.Dtos.SelectionAndPricing
{
    public class ControlListItem : IEquatable<ControlListItem>
    {
        public string ControlName { get; set; }
        public string ControlParentName { get; set; }
        public string ControlGrandparentName { get; set; }
        public string ControlUIType { get; set; }
        public int StepId { get; set; }
        public int ParentStepId { get; set; } = -1;
        public int GrandparentStepId { get; set; } = -1;

        public bool Equals(ControlListItem other)
        {
            if (other is null) return false;

            if (!CompareStringProp(ControlName, other.ControlName)) return false;
            if (!CompareStringProp(ControlParentName, other.ControlParentName)) return false;
            if (!CompareStringProp(ControlGrandparentName, other.ControlGrandparentName)) return false;
            if (!CompareStringProp(ControlUIType, other.ControlUIType)) return false;
            if (!CompareIntProp(StepId, other.StepId)) return false;
            if (!CompareIntProp(ParentStepId, other.ParentStepId)) return false;
            if (!CompareIntProp(GrandparentStepId, other.GrandparentStepId)) return false;

            return true;
        }

        private bool CompareStringProp(string? myPropVal, string? otherPropVal)
        {
            if (myPropVal is null && otherPropVal is null) return true;
            if (myPropVal is null && otherPropVal is not null) return false;
            if (myPropVal is not null && otherPropVal is null) return false;
            if (!myPropVal.Equals(otherPropVal)) return false;

            return true;
        }

        private bool CompareIntProp(int? myPropVal, int? otherPropVal)
        {
            if (myPropVal is null && otherPropVal is null) return true;
            if (myPropVal is null && otherPropVal is not null) return false;
            if (myPropVal is not null && otherPropVal is null) return false;
            if (!myPropVal.Equals(otherPropVal)) return false;

            return true;
        }

        public override bool Equals(object obj) => Equals(obj as ControlListItem);

        public override int GetHashCode() => (ControlName, ControlParentName, ControlGrandparentName, StepId, ParentStepId, GrandparentStepId).GetHashCode();
    }
}
