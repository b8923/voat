//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Voat.Data.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class UserContribution
    {
        public int ID { get; set; }
        public string UserName { get; set; }
        public bool IsReceived { get; set; }
        public int ContentType { get; set; }
        public int VoteStatus { get; set; }
        public int VoteCount { get; set; }
        public double VoteValue { get; set; }
        public System.DateTime ValidThroughDate { get; set; }
        public System.DateTime LastUpdateDate { get; set; }
    }
}
