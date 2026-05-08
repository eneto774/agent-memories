using Microsoft.SemanticKernel;

namespace AgentService.Application.Services;

public interface IKernelFactory
{
    Kernel CreateForUser(Guid userId);
}
