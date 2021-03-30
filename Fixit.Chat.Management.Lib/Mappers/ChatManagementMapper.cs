using System;
using System.Collections.Generic;
using AutoMapper;
using Fixit.Chat.Management.Lib.Models.Messages.Operations;
using Fixit.Core.DataContracts.Notifications.Enums;
using Fixit.Core.DataContracts.Notifications.Operations;
using Fixit.Core.DataContracts.Notifications.Payloads;
using Fixit.Core.DataContracts.Users;

namespace Fixit.Chat.Management.Lib.Mappers
{
  public class ChatManagementMapper : Profile
  {
    public ChatManagementMapper()
    {
      CreateMap<UserMessageCreateRequestDto, EnqueueNotificationRequestDto>()
        .ForMember(notification => notification.Recipients, opts => opts.MapFrom(dto => dto != null ? new List<UserSummaryDto>() { dto.Recipient } : default))
        .ForMember(notification => notification.Payload, opts => opts.MapFrom(dto => dto != null ? new ConversationMessagePayloadDto()
        {
          Id = dto.Message.Id,
          SentByUser = dto.Message.CreatedByUser,
          Message = dto.Message.Message,
          Type = dto.Message.Type
        } : default))
        .ForMember(notification => notification.Retries, opts => opts.Ignore())
        .ForMember(notification => notification.Silent, opts => opts.Ignore())
        .ForMember(notification => notification.Tags, opts => opts.Ignore())
        .AfterMap((dto, notification) => notification.Action = NotificationTypes.NewMessage);
    }
  }
}
