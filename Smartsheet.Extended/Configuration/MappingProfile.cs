using AutoMapper;
using NPOI.SS.UserModel;
using Smartsheet.Core.Entities;
using Smartsheet.Extended.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smartsheet.Extended.Configuration
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            //  From Excel Row to Smartsheet Row
            CreateMap<IRow, Row>()
                .ForMember(dest => dest.Cells, opt => opt.MapFrom(src => src.Cells));

            //  From Excel Cell to Smartsheet Cell
            CreateMap<ICell, Cell>()
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => CellExtension.GetCellValue(src)));
        }
    }
}
