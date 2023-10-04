using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using KidsMealApi.DataAccess.Interfaces;

namespace KidsMealApi.DataAccess.Models
{
    public class Kid : IUniqueEntity
    {
        public Kid()
        {
            
        }
        
        public Kid(int id, string firstName, string middleName, string lastName, DateTime birthDate, Gender gender)
        {
            Id = id;
            FirstName = firstName;
            MiddleName = middleName;
            LastName = lastName;
            BirthDate = birthDate;
            Gender = gender;
        }
        
        [Key]
        public int Id { get; set; }
        public string? FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string? LastName { get; set; }
        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";
        public DateTime BirthDate { get; set; }
        public Gender Gender { get; set; }
        [AllowNull]
        public virtual ICollection<KidAssociation> KidAssociations { get; set; }
    }

    public enum Gender 
    { 
        Male, 
        Female 
    };

}