﻿using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiAutores.DTOs;
using WebApiAutores.Entidades;
using WebApiAutores.Filtros;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using WebApiAutores.Utilidades;

namespace WebApiAutores.Controllers.V2
{
    [ApiController]
    //[Route("api/v2/autores")]
    [Route("api/autores")]
    [CabeceraEstaPresente("x-version", "2")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "EsAdmin")]
    public class AutoresController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly IAuthorizationService authorizationService;
        private readonly IConfiguration configuration;

        public AutoresController(ApplicationDbContext context, IMapper mapper, IAuthorizationService authorizationService)
        {
            this.context = context;
            this.mapper = mapper;
            this.authorizationService = authorizationService;
        }

        public ApplicationDbContext Context { get; }


        //[HttpGet("configuraciones")]
        //public ActionResult<string> ObtenerConfiguracion()
        //{
        //    //return configuration["apellido"];
        //    //return configuration["ConnectionStrings:defaultConnection"];
        //    return configuration["apellido"];
        //}


        [HttpGet(Name = "obtenerAutoresv2")] // api/autores
        [AllowAnonymous]
        [ServiceFilter(typeof(HATEOASAutorFilterAttribute))]
        public async Task<ActionResult<List<AutorDTO>>> Get()
        {
            var autores = await context.Autores.ToListAsync();
            autores.ForEach(autor => autor.Nombre = autor.Nombre.ToUpper());
            return mapper.Map<List<AutorDTO>>(autores);

        }


        [HttpGet("{id:int}", Name = "obtenerAutorv2")]
        [AllowAnonymous]
        [ServiceFilter(typeof(HATEOASAutorFilterAttribute))]
        public async Task<ActionResult<AutorDTOconLibros>> Get(int id)
        {
            var autor = await context.Autores
                .Include(autorDB => autorDB.AutoresLibros)
                .ThenInclude(autorLibroDB => autorLibroDB.Libro)    
                .FirstOrDefaultAsync(x => x.Id == id);

            if (autor == null)
            {
                return NotFound();
            }

            var dto = mapper.Map<AutorDTOconLibros>(autor);

            return dto;
        }



        [HttpGet("{nombre}", Name = "obtenerAutorPorNombrev2")]
        public async Task<ActionResult<List<AutorDTO>>> GetPorNombre(string nombre)
        {
            var autores = await context.Autores.Where(autorDB => autorDB.Nombre.Contains(nombre)).ToListAsync();

            return mapper.Map<List<AutorDTO>>(autores);
        }

        [HttpPost(Name = "crearAutorv2")]
        public async Task<ActionResult> Post([FromBody] AutorCreacionDTO autorCreacionDTO)
        {
            var existeAutorConElMismoNombre = await context.Autores.AnyAsync(x => x.Nombre == autorCreacionDTO.Nombre);

            if (existeAutorConElMismoNombre)
            {
                return BadRequest($"Ya existe un autor con el nombre {autorCreacionDTO.Nombre}");
            }

            var autor = mapper.Map<Autor>(autorCreacionDTO);

            context.Add(autor);
            await context.SaveChangesAsync();

            var autorDTO = mapper.Map<AutorDTO>(autor);

            return CreatedAtRoute("obtenerAutorv2", new { id = autor.Id }, autorDTO);
        }

        [HttpPut("{id:int}", Name = "actualizarAutorv2")] // api/autores/1
        public async Task<ActionResult> Put(AutorCreacionDTO autorCreacionDTO, int id)
        {

            var existe = await context.Autores.AnyAsync(x => x.Id == id);

            if (!existe)
            {
                return NotFound();
            }

            var autor = mapper.Map<Autor>(autorCreacionDTO);
            autor.Id = id;

            context.Update(autor);
            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}", Name = "borrarAutorv2")]
        public async Task<ActionResult> Delete(int id)
        {
            var existe = await context.Autores.AnyAsync(x => x.Id == id);

            if (!existe)
            {
                return NotFound();
            }

            context.Remove(new Autor() { Id = id });
            await context.SaveChangesAsync();
            //return Ok();
            return NoContent();
        }
    }
}
