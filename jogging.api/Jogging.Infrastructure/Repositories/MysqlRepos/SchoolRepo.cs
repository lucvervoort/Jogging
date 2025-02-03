using AutoMapper;
using Jogging.Domain.Exceptions;
using Jogging.Domain.Interfaces.RepositoryInterfaces;
using Jogging.Domain.Models;
using Jogging.Persistence.Context;
using Jogging.Persistence.Models.School;
using Microsoft.EntityFrameworkCore;

namespace Jogging.Infrastructure.Repositories.MysqlRepos;

public class SchoolRepo : IGenericRepo<SchoolDom>
{
    private readonly JoggingContext _dbJoggingContext;
    private readonly IMapper _mapper;

    public SchoolRepo(JoggingContext joggingContext, IMapper mapper)
    {
        _dbJoggingContext = joggingContext;
        _mapper = mapper;
    }
    public async Task<List<SchoolDom>> GetAllAsync()
    {
        var schools = await _dbJoggingContext.ExtendedSchools.ToListAsync();

        if (schools == null || !schools.Any())
        {
            throw new SchoolNotFoundException("No schools found");
        }
        var test = _mapper.Map<List<SchoolDom>>(schools);
        return _mapper.Map<List<SchoolDom>>(schools);
    }
    

    public async Task<SchoolDom> GetByIdAsync(int schoolId)
    {
        var school = await GetSchoolById(schoolId);

        if (school == null)
        {
            throw new SchoolNotFoundException("No school found");
        }

        return _mapper.Map<SchoolDom>(school);
    }

    public async Task<SchoolDom> AddAsync(SchoolDom schoolDom)
    {
        var existingSchool = await GetSchoolByName(schoolDom.Name);

        if (existingSchool != null)
        {
            return _mapper.Map<SchoolDom>(existingSchool);
        }

        var newSchool = _mapper.Map<ExtendedSchool>(schoolDom);
        await _dbJoggingContext.SimpleSchools.AddAsync(newSchool);
        await _dbJoggingContext.SaveChangesAsync();



        return _mapper.Map<SchoolDom>(newSchool);
    }

    public async Task<SchoolDom> UpdateAsync(int schoolId, SchoolDom updatedSchool)
    {
        var existingSchool = await GetSchoolById(schoolId);

        if (existingSchool == null)
        {
            throw new SchoolNotFoundException("School not found");
        }

        if (_mapper.Map<SchoolDom>(existingSchool).Equals(updatedSchool))
        {
            return _mapper.Map<SchoolDom>(existingSchool);
        }

        existingSchool.Name = updatedSchool.Name;

        _dbJoggingContext.SimpleSchools.Update(existingSchool);
        await _dbJoggingContext.SaveChangesAsync();

        return _mapper.Map<SchoolDom>(existingSchool);
    }

    public async Task<SchoolDom> UpsertAsync(int? schoolId, SchoolDom updatedSchool)
    {
        SimpleSchool? currentSchool;
        if (schoolId.HasValue)
        {
            currentSchool = await GetSchoolById(schoolId.Value);
        }
        else
        {
            currentSchool = await GetSchoolByName(updatedSchool.Name);
        }

        if (currentSchool != null)
        {
            var currentSchoolDom = _mapper.Map<SchoolDom>(currentSchool);
            if (currentSchoolDom.Equals(updatedSchool))
            {
                return currentSchoolDom;
            }

            currentSchool.Name = updatedSchool.Name;

            _dbJoggingContext.SimpleSchools.Update(currentSchool);
            await _dbJoggingContext.SaveChangesAsync();

            return _mapper.Map<SchoolDom>(currentSchool);
        }

        return await AddAsync(updatedSchool);
    }

    public async Task DeleteAsync(int id)
    {
        var school = await GetSchoolById(id);

        if (school == null)
        {
            throw new SchoolNotFoundException("School not found");
        }

        _dbJoggingContext.SimpleSchools.Remove(school);
        await _dbJoggingContext.SaveChangesAsync();
    }

    private async Task<SimpleSchool?> GetSchoolById(int schoolId)
    {
        return await _dbJoggingContext.SimpleSchools
            .Where(c => c.SchoolId == schoolId)
            .SingleOrDefaultAsync();
    }

    private async Task<SimpleSchool?> GetSchoolByName(string schoolName)
    {
        return await _dbJoggingContext.SimpleSchools
            .Where(c => c.Name == schoolName)
            .FirstOrDefaultAsync();
    }
}