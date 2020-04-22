export class sutTest
{
    constructor(
        public name: string,
        public methodName: string,
        public className: string,
        public fn: any,
        public success: boolean,
        public result: any
    ) { }
}

export class sut
{
    private ignoredClasses: Array<string> = [];
    private tests: { [name: string]: sutTest; } = {};
    private totalTests: number = 0;
    private countStarted: number = 0;
    private countFinished: number = 0;
    private countSuccess: number = 0;
    private countFailures: number = 0;
    private runSuccess: boolean = false;
    private runStarted: boolean = false;
    private runFinished: boolean = false;

    ignoreFailureForClass(testClass: any)
    {
        this.ignoredClasses.push(testClass.name);
    }

    isFailureIgnoredClass(className: string): boolean
    {
        return this.ignoredClasses.indexOf(className) >= 0;
    }

    addTestClass(testClass: any): void
    {
        for (var member in testClass.prototype) {
            if (typeof testClass.prototype[member] == 'function') {
                let methodName = member;
                let className = testClass.name;
                let name = className + '_' + methodName;
                this.tests[name] = new sutTest(name, methodName, className,testClass.prototype[member], false, null);
                this.totalTests++;
            }
        }
    }

    run()
    {
        this.runStarted = true;
        this.runSuccess = true;
        for (var name in this.tests) {
            let result;
            this.countStarted++;
            try {
                result = this.tests[name].fn();
            } catch (ex) {
                result = ex;
            }
            this.countFinished++;
            this.tests[name].result = result;
            if (result === undefined || result === '') {
                this.tests[name].success = true;
                if (!(this.isFailureIgnoredClass(this.tests[name].className))) {
                    this.countSuccess++;
                }
            } else {
                this.tests[name].success = false;
                if (!(this.isFailureIgnoredClass(this.tests[name].className))) {
                    console.log(name, result);
                    this.countFailures++;
                    this.runSuccess = false;
                }
            }
        }
        this.runFinished = true;
    }

    getResult()
    {
        return {
            'runSuccess': this.runSuccess,
            'runStarted': this.runStarted,
            'runFinished': this.runFinished,
            'countSuccess': this.countSuccess,
            'countFailures': this.countFailures,
            'totalTests': this.totalTests,
            'tests': this.tests,
        }
    }
}