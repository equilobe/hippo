import {
    ChannelItem,
    ChannelService,
    JobStatus,
    JobStatusService,
    JobsService,
    RevisionItem,
} from 'src/app/core/api/v1';
import { Component, Input, OnChanges, OnDestroy, OnInit } from '@angular/core';
import {
    faCircle,
    faNetworkWired,
    faTimesCircle,
} from '@fortawesome/free-solid-svg-icons';

import { ComponentTypes } from 'src/app/_helpers/constants';
import { Router } from '@angular/router';
import TimeAgo from 'javascript-time-ago';
import en from 'javascript-time-ago/locale/en';

@Component({
    selector: 'app-channel-overview',
    templateUrl: './overview.component.html',
    styleUrls: ['./overview.component.css'],
})
export class OverviewComponent implements OnChanges, OnInit, OnDestroy {
    @Input() channelId = '';
    error: any = null;
    channel!: ChannelItem;
    channelStatus!: JobStatus;
    activeRevision!: RevisionItem | undefined;
    publishedAt: string | null | undefined;
    icons = { faCircle, faTimesCircle, faNetworkWired };
    types = ComponentTypes;
    jobStatuses = JobStatus;
    protocol = window.location.protocol;
    loading = false;
    timeAgo: any;

    interval: any = null;
    timeInterval = 5000;

    constructor(
        private readonly channelService: ChannelService,
        private readonly jobStatusService: JobStatusService,
        private readonly jobsService: JobsService,
        private router: Router
    ) {
        TimeAgo.addDefaultLocale(en);
        this.timeAgo = new TimeAgo('en-US');
    }

    ngOnInit(): void {
        this.getJobStatus();

        this.interval = setInterval(() => {
            this.getJobStatus();
        }, this.timeInterval);
    }

    ngOnChanges(): void {
        this.refreshData();
    }

    ngOnDestroy(): void {
        clearInterval(this.interval);
    }

    getJobStatus(): void {
        this.jobStatusService
            .apiJobstatusChannelIdGet(this.channelId)
            .subscribe((res) => (this.channelStatus = res.status));
    }

    startJob(): void {
        this.loading = true;
        this.jobsService.apiJobsIdStartPost(this.channelId).subscribe({
            next: () => {
                this.loading = false;
                this.getJobStatus();
            },
            error: (err) => {
                this.error = err;
                this.loading = false;
            },
        });
    }

    stopJob(): void {
        this.loading = true;
        this.jobsService.apiJobsIdStopPost(this.channelId).subscribe({
            next: () => {
                this.loading = false;
                this.getJobStatus();
            },
            error: (err) => {
                this.error = err;
                this.loading = false;
            },
        });
    }

    switchJobStatus(): void {
        if (this.channelStatus === this.jobStatuses.Running) {
            this.stopJob();
        } else if (this.channelStatus === this.jobStatuses.Dead) {
            this.startJob();
        }
    }

    getStatusColor(status: JobStatus | undefined) {
        switch (status) {
            case JobStatus.Unknown:
                return 'gray';
            case JobStatus.Pending:
                return 'yellow';
            case JobStatus.Running:
                return 'green';
            case JobStatus.Dead:
                return 'red';
            default:
                return 'gray';
        }
    }

    refreshData() {
        this.loading = true;
        this.channelService.apiChannelIdGet(this.channelId).subscribe({
            next: (channel) => {
                !channel
                    ? this.router.navigate(['/404'])
                    : (this.channel = channel);
                this.activeRevision = channel.activeRevision;
                if (channel.lastPublishAt) {
                    const date = new Date(channel.lastPublishAt);
                    this.publishedAt = this.timeAgo.format(date);
                }
                this.loading = false;
            },
            error: (error) => {
                this.error = error;
                this.loading = false;
            },
        });
    }

    getRedirectRoute(route: string): string {
        if (route) {
            if (route.slice(-3) === '...') {
                return route.slice(0, -3);
            } else {
                return route;
            }
        } else return '';
    }
}
