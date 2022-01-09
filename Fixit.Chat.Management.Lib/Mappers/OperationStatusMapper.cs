using AutoMapper;
using Fixit.Core.DataContracts.Chat;
using Fixit.Core.DataContracts.Chat.Messages;
using Fixit.Core.DataContracts;

namespace Fixit.Chat.Management.Lib.Mappers
{
  public class OperationStatusMapper : Profile
  {
    public OperationStatusMapper()
    {
      CreateMap<OperationStatus, OperationStatusWithObject<ConversationMessageDto>>()
        .ForMember(dto => dto.IsOperationSuccessful, opts => opts.MapFrom(operationStatus => operationStatus.IsOperationSuccessful))
        .ForMember(dto => dto.Error, opts => opts.MapFrom(operationStatus => operationStatus.Error))
        .ForMember(dto => dto.OperationException, opts => opts.MapFrom(operationStatus => operationStatus.OperationException))
        .ReverseMap();

      CreateMap<OperationStatus, OperationStatusWithObject<ConversationDto>>()
        .ForMember(dto => dto.IsOperationSuccessful, opts => opts.MapFrom(operationStatus => operationStatus != null && operationStatus.IsOperationSuccessful))
        .ForMember(dto => dto.OperationException, opts => opts.MapFrom(operationStatus => operationStatus != null ? operationStatus.OperationException : default))
        .ForMember(dto => dto.Error, opts => opts.MapFrom(operationStatus => operationStatus != null ? operationStatus.Error : default));

      CreateMap<OperationStatus, OperationStatusWithObject<long>>()
        .ForMember(dto => dto.IsOperationSuccessful, opts => opts.MapFrom(operationStatus => operationStatus != null && operationStatus.IsOperationSuccessful))
        .ForMember(dto => dto.OperationException, opts => opts.MapFrom(operationStatus => operationStatus != null ? operationStatus.OperationException : default))
        .ForMember(dto => dto.Error, opts => opts.MapFrom(operationStatus => operationStatus != null ? operationStatus.Error : default));
    }
  }
}
