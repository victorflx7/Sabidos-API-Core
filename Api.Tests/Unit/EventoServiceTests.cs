//using System;
//using System.Linq;
//using System.Threading.Tasks;
//using AutoMapper;
//using Microsoft.EntityFrameworkCore;
//using SabidosAPI_Core.Data;
//using SabidosAPI_Core.DTOs;
//using SabidosAPI_Core.Models;
//using SabidosAPI_Core.Services;
//using Xunit;

//namespace Api.Tests.Unit
//{
//    public class EventoServiceTests
//    {
//        private static IMapper BuildMapper()
//        {
//            var config = new MapperConfiguration(cfg => cfg.AddMaps(typeof(Evento).Assembly));
//            return config.CreateMapper();
//        }

//        private static AppDbContext BuildContext(string dbName)
//        {
//            var options = new DbContextOptionsBuilder<AppDbContext>()
//                .UseInMemoryDatabase(dbName)
//                .Options;
//            return new AppDbContext(options);
//        }

//        [Fact]
//        public async Task CreateEventoAsync_SavesEventoAndReturnsDto()
//        {
//            var dbName = Guid.NewGuid().ToString();
//            await using var context = BuildContext(dbName);
//            var mapper = BuildMapper();
//            var service = new EventoService(context, mapper);

//            var dto = new EventoResponseDto
//            {
//                TitleEvent = "Evento de Teste",
//                DataEvento = DateTime.UtcNow
//            };

//            var result = await service.CreateEventoAsync(dto, "author-uid-1");

//            Assert.NotNull(result);
//            Assert.Equal(dto.TitleEvent, result.TitleEvent);

//            var saved = await context.Eventos.FirstOrDefaultAsync(e => e.TitleEvent == dto.TitleEvent);
//            Assert.NotNull(saved);
//            Assert.Equal("author-uid-1", saved.AuthorUid);
//        }

//        [Fact]
//        public async Task GetAllEventosAsync_FiltersByUser_ReturnsOnlyOwnerEventos()
//        {
//            var dbName = Guid.NewGuid().ToString();
//            await using var context = BuildContext(dbName);
//            var mapper = BuildMapper();

//            context.Eventos.AddRange(
//                new Evento { TitleEvent = "E1", DataEvento = DateTime.UtcNow.AddDays(-1), AuthorUid = "uid1" },
//                new Evento { TitleEvent = "E2", DataEvento = DateTime.UtcNow, AuthorUid = "uid2" }
//            );
//            await context.SaveChangesAsync();

//            var service = new EventoService(context, mapper);

//            var listUid1 = await service.GetAllEventosAsync("uid1");
//            var listUid2 = await service.GetAllEventosAsync("uid2");
//            var listAll = await service.GetAllEventosAsync(null);

//            Assert.Single(listUid1);
//            Assert.Equal("E1", listUid1.First().TitleEvent);

//            Assert.Single(listUid2);
//            Assert.Equal("E2", listUid2.First().TitleEvent);

//            Assert.Equal(2, listAll.Count);
//        }

//        [Fact]
//        public async Task GetEventosByIdAsync_ReturnsNull_WhenNotFound()
//        {
//            var dbName = Guid.NewGuid().ToString();
//            await using var context = BuildContext(dbName);
//            var mapper = BuildMapper();
//            var service = new EventoService(context, mapper);

//            var result = await service.GetEventosByIdAsync(9999);

//            Assert.Null(result);
//        }

//        [Fact]
//        public async Task UpdateEventoAsync_UpdatesExisting_ReturnsDto()
//        {
//            var dbName = Guid.NewGuid().ToString();
//            await using var context = BuildContext(dbName);
//            var mapper = BuildMapper();

//            var existing = new Evento
//            {
//                TitleEvent = "Original",
//                DataEvento = DateTime.UtcNow.AddDays(-2),
//                AuthorUid = "uidX"
//            };
//            context.Eventos.Add(existing);
//            await context.SaveChangesAsync();

//            var service = new EventoService(context, mapper);

//            var updateDto = new EventoResponseDto
//            {
//                TitleEvent = "Atualizado",
//                DataEvento = existing.DataEvento!.Value
//            };

//            var updated = await service.UpdateEventoAsync(existing.Id, updateDto);

//            Assert.NotNull(updated);
//            Assert.Equal("Atualizado", updated.TitleEvent);

//            var saved = await context.Eventos.FindAsync(existing.Id);
//            Assert.NotNull(saved);
//            Assert.Equal("Atualizado", saved!.TitleEvent);
//        }

//        [Fact]
//        public async Task DeleteEventoAsync_RemovesEvento_ReturnsTrue_WhenExists()
//        {
//            var dbName = Guid.NewGuid().ToString();
//            await using var context = BuildContext(dbName);
//            var mapper = BuildMapper();

//            var evento = new Evento
//            {
//                TitleEvent = "ToDelete",
//                DataEvento = DateTime.UtcNow,
//                AuthorUid = "uidDel"
//            };
//            context.Eventos.Add(evento);
//            await context.SaveChangesAsync();

//            var service = new EventoService(context, mapper);

//            var deleted = await service.DeleteEventoAsync(evento.Id);

//            Assert.True(deleted);
//            var exists = await context.Eventos.AnyAsync(e => e.Id == evento.Id);
//            Assert.False(exists);
//        }

//        [Fact]
//        public async Task GetEventosCountByUserAsync_ReturnsCorrectCount()
//        {
//            var dbName = Guid.NewGuid().ToString();
//            await using var context = BuildContext(dbName);
//            var mapper = BuildMapper();

//            context.Eventos.AddRange(
//                new Evento { TitleEvent = "A", DataEvento = DateTime.UtcNow, AuthorUid = "count-uid" },
//                new Evento { TitleEvent = "B", DataEvento = DateTime.UtcNow, AuthorUid = "count-uid" },
//                new Evento { TitleEvent = "C", DataEvento = DateTime.UtcNow, AuthorUid = "other" }
//            );
//            await context.SaveChangesAsync();

//            var service = new EventoService(context, mapper);

//            var count = await service.GetEventosCountByUserAsync("count-uid");

//            Assert.Equal(2, count);
//        }
//    }
//}