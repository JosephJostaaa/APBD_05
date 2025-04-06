using Tutorial3.Models;

public class EmpDeptSalgradeTests
{
    // 1. Simple WHERE filter
    // SQL: SELECT * FROM Emp WHERE Job = 'SALESMAN';
    [Fact]
    public void ShouldReturnAllSalesmen()
    {
        var emps = Database.GetEmps();

        List<Emp> result = (from emp in emps
            where emp.Job.Equals("SALESMAN")
            select emp).ToList();

        Assert.Equal(2, result.Count);
        Assert.All(result, e => Assert.Equal("SALESMAN", e.Job));
    }

    // 2. WHERE + OrderBy
    // SQL: SELECT * FROM Emp WHERE DeptNo = 30 ORDER BY Sal DESC;
    [Fact]
    public void ShouldReturnDept30EmpsOrderedBySalaryDesc()
    {
        var emps = Database.GetEmps();

        List<Emp> result = (from emp in emps
            where emp.DeptNo == 30
            orderby emp.Sal descending
            select emp).ToList(); 

        Assert.Equal(2, result.Count);
        Assert.True(result[0].Sal >= result[1].Sal);
    }

    // 3. Subquery using LINQ (IN clause)
    // SQL: SELECT * FROM Emp WHERE DeptNo IN (SELECT DeptNo FROM Dept WHERE Loc = 'CHICAGO');
    [Fact]
    public void ShouldReturnEmployeesFromChicago()
    {
        var emps = Database.GetEmps();
        var depts = Database.GetDepts();

        List<Emp> result = (
                from emp in emps 
                where (from dept in depts
                       where dept.Loc.Equals("CHICAGO")
                       select dept.DeptNo).Contains(emp.DeptNo)
                select emp
            ).ToList(); 

        Assert.All(result, e => Assert.Equal(30, e.DeptNo));
    }

    // 4. SELECT projection
    // SQL: SELECT EName, Sal FROM Emp;
    [Fact]
    public void ShouldSelectNamesAndSalaries()
    {
        var emps = Database.GetEmps();

        var result = from emp in emps
                select new
                {
                    emp.EName,
                    emp.Sal,
                }; 
        
         Assert.All(result, r =>
         {
             Assert.False(string.IsNullOrWhiteSpace(r.EName));
             Assert.True(r.Sal > 0);
         });
    }

    // 5. JOIN Emp to Dept
    // SQL: SELECT E.EName, D.DName FROM Emp E JOIN Dept D ON E.DeptNo = D.DeptNo;
    [Fact]
    public void ShouldJoinEmployeesWithDepartments()
    {
        var emps = Database.GetEmps();
        var depts = Database.GetDepts();

        var result = 
                from emp in emps
                join dept in depts on emp.DeptNo equals dept.DeptNo
                select new
                {
                    emp.EName,
                    dept.DName,
                    
                }; 

        Assert.Contains(result, r => r.DName == "SALES" && r.EName == "ALLEN");
    }

    // 6. Group by DeptNo
    // SQL: SELECT DeptNo, COUNT(*) FROM Emp GROUP BY DeptNo;
    [Fact]
    public void ShouldCountEmployeesPerDepartment()
    {
        var emps = Database.GetEmps();

         var result = 
                 from emp in emps
                 group emp by emp.DeptNo into deptGroup
                 select new
                 {
                     DeptNo = deptGroup.Key,
                     Count = deptGroup.Count()
                 }; 
        
         Assert.Contains(result, g => g.DeptNo == 30 && g.Count == 2);
    }

    // 7. SelectMany (simulate flattening)
    // SQL: SELECT EName, Comm FROM Emp WHERE Comm IS NOT NULL;
    [Fact]
    public void ShouldReturnEmployeesWithCommission()
    {
        var emps = Database.GetEmps();

         var result = 
                 from emp in emps
                 where emp.Comm is not null
                 select new
                 {
                    emp.EName,
                    emp.Comm
                 }; 
        
         Assert.All(result, r => Assert.NotNull(r.Comm));
    }

    // 8. Join with Salgrade
    // SQL: SELECT E.EName, S.Grade FROM Emp E JOIN Salgrade S ON E.Sal BETWEEN S.Losal AND S.Hisal;
    [Fact]
    public void ShouldMatchEmployeeToSalaryGrade()
    {
        var emps = Database.GetEmps();
        var grades = Database.GetSalgrades();

        var result =
            from emp in emps
            from grade in grades
            where emp.Sal <= grade.Hisal && emp.Sal >= grade.Losal
            select new
            {
                emp.EName,
                grade.Grade
            };
        
         Assert.Contains(result, r => r.EName == "ALLEN" && r.Grade == 3);
    }

    // 9. Aggregation (AVG)
    // SQL: SELECT DeptNo, AVG(Sal) FROM Emp GROUP BY DeptNo;
    [Fact]
    public void ShouldCalculateAverageSalaryPerDept()
    {
        var emps = Database.GetEmps();
        
        var result = 
                from emp in emps
                group emp by emp.DeptNo into deptGroup
                select new
                {
                    AvgSal = deptGroup.Average(e => e.Sal),
                    DeptNo = deptGroup.Key
                }; 
        
        Assert.Contains(result, r => r.DeptNo == 30 && r.AvgSal > 1000);
    }

    // 10. Complex filter with subquery and join
    // SQL: SELECT E.EName FROM Emp E WHERE E.Sal > (SELECT AVG(Sal) FROM Emp WHERE DeptNo = E.DeptNo);
    [Fact]
    public void ShouldReturnEmployeesEarningMoreThanDeptAverage()
    {
        var emps = Database.GetEmps();

        var result = 
                from emp in emps
                where emp.Sal > (
                    from emp1 in emps
                    where emp.DeptNo == emp1.DeptNo
                    select emp1.Sal).Average()
                    select emp.EName; 
        
        Assert.Contains("ALLEN", result);
    }
}
