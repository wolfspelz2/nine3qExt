const $ = require('jquery');
import { sut } from './sut';

export class sutGui
{
    render(s: sut, parent: HTMLElement)
    {
        let result = s.getResult();

        let eTop = <HTMLDivElement>$('<div class="sut" />')[0];
        if (result.runSuccess) {
            $(eTop).addClass('sut-success');
        } else {
            $(eTop).addClass('sut-failure');
        }

        let eTotals = <HTMLDivElement>$('<div class="sut-totals" />')[0];

        {
            let e = <HTMLDivElement>$('<div class="sut-total sut-total-runSuccess">Success</div>')[0];
            e.append($('<div class="sut-label" />')[0]);
            e.append($('<div class="sut-value">' + (result.runSuccess ? 'true' : 'false') + '</div>')[0]);
            eTotals.append(e);
        }

        {
            let e = <HTMLDivElement>$('<div class="sut-total sut-total-countSuccess">Successful</div>')[0];
            e.append($('<div class="sut-label" />')[0]);
            e.append($('<div class="sut-value">' + result.countSuccess + '</div>')[0]);
            eTotals.append(e);
        }

        {
            let e = <HTMLDivElement>$('<div class="sut-total sut-total-countFailures">Failures</div>')[0];
            e.append($('<div class="sut-label" />')[0]);
            e.append($('<div class="sut-value">' + result.countFailures + '</div>')[0]);
            eTotals.append(e);
        }

        eTop.append(eTotals);

        let eList = <HTMLDivElement>$('<div class="sut-testlist" />')[0];
        for (var name in result.tests) {
            let eTest = <HTMLDivElement>$('<div class="sut-test" />')[0];
            if (result.tests[name].success) {
                $(eTest).addClass('sut-test-success');
            } else {
                $(eTest).addClass('sut-test-failure');
            }

            {
                let e = <HTMLDivElement>$('<div class="sut-class" />')[0];
                $(e).text(result.tests[name].className);
                eTest.append(e);
            }

            {
                let e = <HTMLDivElement>$('<div class="sut-name" />')[0];
                $(e).text(result.tests[name].methodName);
                eTest.append(e);
            }

            {
                let e = <HTMLDivElement>$('<div class="sut-result" />')[0];
                $(e).text(result.tests[name].result);
                eTest.append(e);
            }

            eList.append(eTest);
        });

        eTop.append(eList);

        parent.append(eTop);
    }
}