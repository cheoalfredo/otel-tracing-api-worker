namespace TracingWorker.Domain.Entities
{
    public class Person
    {
        public Person()
        {
        }

        public Person(string firstName, string lastName,
            string email)
        {
            this.FirstName = firstName;
            this.LastName = lastName;
            this.Email = email;
        }

        public Guid Id { get; set; }
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public string Email { get; set; } = default!;

    }


}
