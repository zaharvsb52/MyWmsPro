select *
from   wmsplace p
where  p.placecode in (
    select p.placecode
    from   wmsplace p
           inner join wmssegment s on s.segmentcode = p.segmentcode_r
           inner join wmsarea a on a.areacode = s.areacode_r       
    where  a.warehousecode_r in (
      select a.warehousecode_r
      from   wmsplace p
             inner join wmssegment s on s.segmentcode = p.segmentcode_r
             inner join wmsarea a on a.areacode = s.areacode_r       
      where  p.placecode = 'T1001223')
      ) and p.placecode not in (select p.placecode from wmstransporttask t 
                                       where t.ttaskcurrentplace = p.placecode 
                                       or t.ttaskstartplace = p.placecode 
                                       or t.ttasknextplace = p.placecode
                                       or t.ttaskfinishplace = p.placecode)
