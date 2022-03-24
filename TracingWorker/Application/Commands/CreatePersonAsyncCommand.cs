using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TracingWorker.Domain.Entities;
using TracingWorker.Infrastructure;

namespace TracingWorker.Application.Commands
{
    public record CreatePersonAsyncCommand(string FirstName, string LastName, string Email) : IRequest;

    public class CreatePersonAsyncCommandHandler : AsyncRequestHandler<CreatePersonAsyncCommand>
    {
        readonly PersistenceContext _context;
        public CreatePersonAsyncCommandHandler(PersistenceContext context) => _context = context;
        
        protected override async Task Handle(CreatePersonAsyncCommand request, CancellationToken cancellationToken)
        {
            await _context.Person.AddAsync(new Person
            {
                FirstName = request.FirstName,
                LastName = request.LastName,                
                Email = request.Email
            });         
        }
    }
}