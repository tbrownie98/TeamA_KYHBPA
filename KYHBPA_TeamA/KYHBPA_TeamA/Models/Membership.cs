﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KYHBPA_TeamA.Models
{
    public class Membership
    {
        public int ID { get; set; }
        public DateTime? DateofBirth { get; set; }
        public DateTime? MembershipEnrollment { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public string LicenseNumber { get; set; }
        public bool IsOwner { get; set; }
        public bool IsTrainer { get; set; }
        public bool IsOwnerAndTrainer { get; set; }
        public bool AgreedToTerms { get; set; }
        public string Signature { get; set; }
        public string Affiliation { get; set; }
        public string ManagingPartner { get; set; }
    }
}