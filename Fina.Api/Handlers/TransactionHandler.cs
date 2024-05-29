using Fina.Api.Data;
using Fina.Core.Common;
using Fina.Core.Enums;
using Fina.Core.Handlers;
using Fina.Core.Models;
using Fina.Core.Requests.Transactions;
using Fina.Core.Responses;
using Microsoft.EntityFrameworkCore;

namespace Fina.Api.Handlers;

public class TransactionHandler(AppDbContext dbContext) : ITransactionHandler
{
    public async Task<Response<Transaction?>> CreateAsync(CreateTransactionRequest request)
    {
        if (request is { Type: ETransactionType.Withdraw, Amount: >= 0 })
            request.Amount *= -1;

        try
        {
            var transaction = new Transaction()
            {
                UserId = request.UserId,
                CategoryId = request.CategoryId,
                CreatedAt = DateTime.Now,
                Amount = request.Amount,
                PaidOrReceivedAt = request.PaidOrReceivedAt,
                Title = request.Title,
                Type = request.Type
            };

            await dbContext.Transactions.AddAsync(transaction);
            await dbContext.SaveChangesAsync();

            return new Response<Transaction?>(transaction, 201, "Transação criada com sucesso!");
        }
        catch
        {
            return new Response<Transaction?>(null, 500, "Não foi possível criar a transação!");
        }
    }

    public async Task<Response<Transaction?>> UpdateAsync(UpdateTransactionRequest request)
    {
        if (request is { Type: ETransactionType.Withdraw, Amount: >= 0 })
            request.Amount *= -1;

        try
        {
            var transaction = await dbContext.Transactions
                                             .FirstOrDefaultAsync(t =>
                                                 t.Id == request.Id && t.UserId == request.UserId);

            if (transaction is null)
                return new Response<Transaction?>(null, 404, "Transação não encotrada!");

            transaction.CategoryId = request.CategoryId;
            transaction.Amount = request.Amount;
            transaction.PaidOrReceivedAt = request.PaidOrReceivedAt;
            transaction.Title = request.Title;
            transaction.Type = request.Type;


            dbContext.Transactions.Update(transaction);
            await dbContext.SaveChangesAsync();

            return new Response<Transaction?>(transaction, message:"Transação atualizada com sucesso!");
        }
        catch
        {
            return new Response<Transaction?>(null, 500, "Não foi possível atualizar a transação!");
        }
    }

    public async Task<Response<Transaction?>> DeleteAsync(DeleteTransactionRequest request)
    {
        try
        {
            var transaction = await dbContext.Transactions
                                             .FirstOrDefaultAsync(t =>
                                                 t.Id == request.Id && t.UserId == request.UserId);

            if (transaction is null)
                return new Response<Transaction?>(null, 404, "Transação não encotrada!");

            dbContext.Transactions.Remove(transaction);
            await dbContext.SaveChangesAsync();

            return new Response<Transaction?>(transaction, message:"Transação excluída com sucesso!");
        }
        catch
        {
            return new Response<Transaction?>(null, 500, "Não foi possível excluir a transação!");
        }
    }

    public async Task<Response<Transaction?>> GetByIdAsync(GetTransactionByIdRequest request)
    {
        try
        {
            var transaction = await dbContext.Transactions
                                             .FirstOrDefaultAsync(t =>
                                                 t.Id == request.Id && t.UserId == request.UserId);

            return transaction is null
                ?new Response<Transaction?>(null, 404, "Transação não encontrada!")
                : new Response<Transaction?>(transaction);
        }
        catch
        {
            return new Response<Transaction?>(null, 500, "Não foi possível encontrar a transação!");
        }
    }

    public async Task<PagedResponse<List<Transaction>?>> GetByPeriodAsync(GetTransactionByPeriodRequest request)
    {
        try
        {
            request.StartDate ??= DateTime.Now.GetFirstDay();
            request.EndDate ??= DateTime.Now.GetLastDay();

            var query = dbContext.Transactions
                                 .AsNoTracking()
                                 .Where(x =>
                                     x.PaidOrReceivedAt >= request.StartDate &&
                                     x.PaidOrReceivedAt <= request.EndDate &&
                                     x.UserId == request.UserId)
                                 .OrderBy(x => x.PaidOrReceivedAt);

            var transactions = await query
                                     .Skip((request.PageNumber - 1) * request.PageSize)
                                     .Take(request.PageSize)
                                     .ToListAsync();

            var count = await query.CountAsync();

            return new PagedResponse<List<Transaction>?>(transactions, count, request.PageNumber, request.PageSize);
        }
        catch
        {
            return new PagedResponse<List<Transaction>?>(null, 500, "Não foi possível recuperar as transações!");
        }
    }
}