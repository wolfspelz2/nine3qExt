export class sut
{
    private ignoredClasses: Array<string> = [];
    private names: Array<string> = [];
    private tests: { [name: string]: any; } = {};
    private functions: { [name: string]: any; } = {};
    private results: { [name: string]: boolean; } = {};
    private messages: { [name: string]: any; } = {};
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
                let name = className + '.' + methodName;
                this.names.push(name);
                this.tests[name] = { 'methodName': methodName, 'className': className };
                this.functions[name] = testClass.prototype[member];
                this.totalTests++;
            }
        }
    }

    run()
    {
        this.runStarted = true;
        this.runSuccess = true;
        for (var name in this.functions) {
            let message;
            this.countStarted++;
            try {
                message = this.functions[name]();
            } catch (ex) {
                message = ex;
            }
            this.countFinished++;
            this.messages[name] = message;
            if (message === undefined || message === '') {
                this.results[name] = true;
                if (!(this.isFailureIgnoredClass(this.tests[name].className))) {
                    this.countSuccess++;
                }
            } else {
                this.results[name] = false;
                if (!(this.isFailureIgnoredClass(this.tests[name].className))) {
                    this.countFailures++;
                    this.runSuccess = false;
                }
            }
        }
        this.runFinished = true;
    }

    getResults()
    {
        return {
            'runSuccess': this.runSuccess,
            'runStarted': this.runStarted,
            'runFinished': this.runFinished,
            'countSuccess': this.countSuccess,
            'countFailures': this.countFailures,
            'names': this.names,
            'results': this.results,
            'messages': this.messages,
            'totalTests': this.totalTests,
            'tests': this.tests,
        }
    }
}