const $ = require('jquery');
import { sut } from './sut';

export class sutAwsome
{
    render(s: sut, parent: HTMLElement)
    {
        let results = s.getResults();

        let eTop = <HTMLDivElement>$('<div class="sut" />')[0];
        if (results.runSuccess) {
            $(eTop).addClass('sut-success');
        } else {
            $(eTop).addClass('sut-failure');
        }

        let eTotals = <HTMLDivElement>$('<div class="sut-totals" />')[0];

        {
            let e = <HTMLDivElement>$('<div class="sut-total sut-total-runSuccess">Success</div>')[0];
            e.append($('<div class="sut-label" />')[0]);
            e.append($('<div class="sut-value">' + (results.runSuccess ? 'true' : 'false') + '</div>')[0]);
            eTotals.append(e);
        }
        
        {
            let e = <HTMLDivElement>$('<div class="sut-total sut-total-countSuccess">Successful</div>')[0];
            e.append($('<div class="sut-label" />')[0]);
            e.append($('<div class="sut-value">' + results.countSuccess + '</div>')[0]);
            eTotals.append(e);
        }
        
        {
            let e = <HTMLDivElement>$('<div class="sut-total sut-total-countFailures">Failures</div>')[0];
            e.append($('<div class="sut-label" />')[0]);
            e.append($('<div class="sut-value">' + results.countFailures + '</div>')[0]);
            eTotals.append(e);
        }
        
        eTop.append(eTotals);

        let eList = <HTMLDivElement>$('<div class="sut-testlist" />')[0];
        results.names.forEach(name =>
        {
            let eTest = <HTMLDivElement>$('<div class="sut-test" />')[0];
            if (results.results[name]) {
                $(eTest).addClass('sut-test-success');
            } else {
                $(eTest).addClass('sut-test-failure');
            }

            {
                let e = <HTMLDivElement>$('<div class="sut-class" />')[0];
                $(e).text(results.tests[name].className);
                eTest.append(e);
            }

            {
                let e = <HTMLDivElement>$('<div class="sut-name" />')[0];
                $(e).text(results.tests[name].methodName);
                eTest.append(e);
            }

            {
                let e = <HTMLDivElement>$('<div class="sut-message" />')[0];
                $(e).text(results.messages[name]);
                eTest.append(e);
            }

            eList.append(eTest);
        });

        eTop.append(eList);

        parent.append(eTop);
    }
}