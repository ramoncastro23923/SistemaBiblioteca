using Microsoft.AspNetCore.Identity;
using System.Diagnostics.CodeAnalysis;

namespace SistemaBiblioteca.Services
{
    public class LocalizedIdentityErrorDescriber : IdentityErrorDescriber
    {
        public override IdentityError DefaultError() => new()
        {
            Code = nameof(DefaultError),
            Description = "Ocorreu um erro desconhecido."
        };

        public override IdentityError ConcurrencyFailure() => new()
        {
            Code = nameof(ConcurrencyFailure),
            Description = "Falha de concorrência otimista, o objeto foi modificado."
        };

        public override IdentityError PasswordMismatch() => new()
        {
            Code = nameof(PasswordMismatch),
            Description = "Senha incorreta."
        };

        public override IdentityError InvalidToken() => new()
        {
            Code = nameof(InvalidToken),
            Description = "Token inválido."
        };

        public override IdentityError LoginAlreadyAssociated() => new()
        {
            Code = nameof(LoginAlreadyAssociated),
            Description = "Já existe um usuário com este login."
        };

        public override IdentityError InvalidUserName([NotNull] string userName) => new()
        {
            Code = nameof(InvalidUserName),
            Description = $"Nome de usuário '{userName}' é inválido, pode conter apenas letras ou dígitos."
        };

        public override IdentityError InvalidEmail([NotNull] string email) => new()
        {
            Code = nameof(InvalidEmail),
            Description = $"Email '{email}' é inválido."
        };

        public override IdentityError DuplicateUserName([NotNull] string userName) => new()
        {
            Code = nameof(DuplicateUserName),
            Description = $"Nome de usuário '{userName}' já está em uso."
        };

        public override IdentityError DuplicateEmail([NotNull] string email) => new()
        {
            Code = nameof(DuplicateEmail),
            Description = $"Email '{email}' já está em uso."
        };

        public override IdentityError InvalidRoleName([NotNull] string role) => new()
        {
            Code = nameof(InvalidRoleName),
            Description = $"Nome de perfil '{role}' é inválido."
        };

        public override IdentityError DuplicateRoleName([NotNull] string role) => new()
        {
            Code = nameof(DuplicateRoleName),
            Description = $"Nome de perfil '{role}' já está em uso."
        };

        public override IdentityError UserAlreadyHasPassword() => new()
        {
            Code = nameof(UserAlreadyHasPassword),
            Description = "Usuário já possui uma senha definida."
        };

        public override IdentityError UserLockoutNotEnabled() => new()
        {
            Code = nameof(UserLockoutNotEnabled),
            Description = "Lockout não está habilitado para este usuário."
        };

        public override IdentityError UserAlreadyInRole([NotNull] string role) => new()
        {
            Code = nameof(UserAlreadyInRole),
            Description = $"Usuário já possui o perfil '{role}'."
        };

        public override IdentityError UserNotInRole([NotNull] string role) => new()
        {
            Code = nameof(UserNotInRole),
            Description = $"Usuário não possui o perfil '{role}'."
        };

        public override IdentityError PasswordTooShort(int length) => new()
        {
            Code = nameof(PasswordTooShort),
            Description = $"Senha deve conter pelo menos {length} caracteres."
        };

        public override IdentityError PasswordRequiresNonAlphanumeric() => new()
        {
            Code = nameof(PasswordRequiresNonAlphanumeric),
            Description = "Senha deve conter pelo menos um caractere não alfanumérico."
        };

        public override IdentityError PasswordRequiresDigit() => new()
        {
            Code = nameof(PasswordRequiresDigit),
            Description = "Senha deve conter pelo menos um dígito ('0'-'9')."
        };

        public override IdentityError PasswordRequiresLower() => new()
        {
            Code = nameof(PasswordRequiresLower),
            Description = "Senha deve conter pelo menos uma letra minúscula ('a'-'z')."
        };

        public override IdentityError PasswordRequiresUpper() => new()
        {
            Code = nameof(PasswordRequiresUpper),
            Description = "Senha deve conter pelo menos uma letra maiúscula ('A'-'Z')."
        };
    }
}