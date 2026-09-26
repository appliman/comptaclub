global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading.Tasks;

global using Microsoft.AspNetCore.Components;
global using Microsoft.AspNetCore.Components.Authorization;
global using Microsoft.AspNetCore.Components.Forms;

global using Microsoft.Extensions.DependencyInjection;

global using ComptaClub.Contracts.Models;
global using ComptaClub.Extensions;
global using ComptaClub.Blazor.Extensions;
global using ComptaClub.Blazor.Pages.Layout;
global using ComptaClub.Enums;


global using DialogService = SuperBlazorComponents.Services.SuperDialogService;
global using NotificationService = SuperBlazorComponents.Services.SuperNotificationService;
global using NotificationSeverity = SuperBlazorComponents.Components.Notifications.NotificationSeverity;
global using NotificationMessage = SuperBlazorComponents.Components.Notifications.NotificationMessage;
global using DialogOptions = SuperBlazorComponents.Components.Dialogs.DialogOptions;
global using ConfirmOptions = SuperBlazorComponents.Components.Dialogs.ConfirmOptions;
