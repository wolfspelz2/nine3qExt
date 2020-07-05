import * as $ from 'jquery';
import 'webpack-jquery-ui';
import { as } from '../lib/as';
import { sut } from '../lib/sut';
import { sutGui } from '../lib/sutGui';
import { Window } from './Window';
import { ContentApp } from './ContentApp';
import { TestPayload } from './TestPayload';
import { TestSimpleRpc } from './TestSimpleRpc';

export class TestWindow extends Window
{
    private outElem: HTMLElement;

    constructor(app: ContentApp)
    {
        super(app);
    }

    async show(options: any)
    {
        options.titleText = this.app.translateText('TestWindow.Tests', 'Integration Tests');
        options.resizable = true;

        super.show(options);

        let bottom = as.Int(options.bottom, 400);
        let width = as.Int(options.width, 800);
        let height = as.Int(options.height, 600);
        let onClose = options.onClose;

        if (this.windowElem) {
            let windowElem = this.windowElem;
            let contentElem = this.contentElem;
            $(windowElem).addClass('n3q-testwindow');

            let left = 50;
            let top = this.app.getDisplay().offsetHeight - height - bottom;
            {
                let minTop = 10;
                if (top < minTop) {
                    height -= minTop - top;
                    top = minTop;
                }
            }

            let outElem = <HTMLElement>$('<div class="n3q-base n3q-testwindow-out" data-translate="children" />').get(0);

            $(contentElem).append(outElem);

            this.app.translateElem(windowElem);

            this.outElem = outElem;

            $(windowElem).css({ 'width': width + 'px', 'height': height + 'px', 'left': left + 'px', 'top': top + 'px' });

            this.onClose = async () =>
            {
                this.outElem = null;
                if (onClose) { onClose(); }
            };

            this.runTests();
        }
    }

    runTests()
    {
        var s = new sut();

        s.addTestClass(TestPayload);
        s.addTestClass(TestSimpleRpc);

        s.run().then(() =>
        {
            new sutGui().render(s, this.outElem);
        });
    }
}
