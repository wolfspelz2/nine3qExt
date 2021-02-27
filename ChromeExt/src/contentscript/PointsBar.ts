import * as $ from 'jquery';
import { as } from '../lib/as';
import { IObserver, IObservable } from '../lib/ObservableProperty';
import { ContentApp } from './ContentApp';
import { Participant } from './Participant';
import { Config } from '../lib/Config';
import { Utils } from '../lib/Utils';
import { Environment } from '../lib/Environment';
import { Menu, MenuColumn, MenuItem, MenuHasIcon, MenuOnClickClose, MenuHasCheckbox } from './Menu';
import { PointsGenerator } from './PointsGenerator';

export class PointsBar implements IObserver
{
    private elem: HTMLDivElement;
    private points: number;

    getElem(): HTMLDivElement { return this.elem; }
    getPoints(): number { return this.points; }

    constructor(protected app: ContentApp, private participant: Participant, private display: HTMLElement)
    {
        this.elem = <HTMLDivElement>$('<div class="n3q-base n3q-points" />').get(0);
        $(this.elem).click(() => { this.participant?.select(); });
        $(display).append(this.elem);
    }

    stop()
    {
        // Nothing to do
    }

    updateObservableProperty(name: string, value: string): void
    {
        if (name == 'Points') {
            this.setPoints(as.Int(value, 0));
        }
    }

    setPoints(points: number): void
    {
        this.points = points;
        $(this.elem).empty();
        $(this.elem).attr('title', '' + points);

        let pg = new PointsGenerator(4, 
            Config.get('points.fullLevels', 2), 
            Config.get('points.fractionalLevels', 1)
            );
        let digits = pg.getDigitList(points);
        let parts = pg.getPartsList(digits);
        let stars = parts.map(part => <HTMLDivElement>$('<div class="n3q-base n3q-points-icon n3q-points-icon-' + part + '" />').get(0));
        $(this.elem).append(stars);
    }
}
